using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using SudokuSolver_Api;
using SudokuSolver.Api.Detector;
using SudokuSolver.Api.Solver;
using SudokuSolver.Api.Solver.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPredictionEnginePool<SudokuDetector.ModelInput, SudokuDetector.ModelOutput>()
    .FromFile("SudokuDetector.mlnet");

builder.Services.AddPredictionEnginePool<DigitDetector.ModelInput, DigitDetector.ModelOutput>()
    .FromFile("DigitDetector.mlnet");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();
builder.Services.AddCors(opts => opts.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseAntiforgery();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();


app.MapPost("/detect-sudoku",
    async (PredictionEnginePool<SudokuDetector.ModelInput, SudokuDetector.ModelOutput> predictionEnginePool, [FromForm]string uploadedImage) =>
    {
        var bytes = Convert.FromBase64String(uploadedImage.Replace("data:image/webp;base64,", ""));
        var contents = new StreamContent(new MemoryStream(bytes));
        var stream = await contents.ReadAsStreamAsync();
        var image = MLImage.CreateFromStream(stream);
        var input = new SudokuDetector.ModelInput
        {
            Image = image,
        };

        var modelOutput = predictionEnginePool.Predict(input);

        var bestScoreIndex = modelOutput.Score.ToList().IndexOf(modelOutput.Score.Max());

        var startBoxIndex = bestScoreIndex * 4;
        var endBoxIndex = startBoxIndex + 4;
        var box = modelOutput.PredictedBoundingBoxes[startBoxIndex..endBoxIndex];
        var left = box[0] / 800;
        var top = box[1] / 600;
        var right = box[2] / 800;
        var bottom = box[3] / 600;

        var result = new DetectedSudoku
        {
            Score = modelOutput.Score[bestScoreIndex],
            Box = [left, top, right, bottom]
        };
        return result;
    })
    .DisableAntiforgery();

app.MapPost("/detect-digit",
    (PredictionEnginePool<DigitDetector.ModelInput, DigitDetector.ModelOutput> predictionEnginePool, [FromForm]string uploadedImage) =>
    {
        var bytes = Convert.FromBase64String(uploadedImage.Replace("data:image/webp;base64,", ""));
        var input = new DigitDetector.ModelInput
        {
            ImageSource = bytes,
        };

        var modelOutput = predictionEnginePool.Predict(input);

        int? result = null;
        if (int.TryParse(modelOutput.PredictedLabel, out var prediction))
        {
            result = prediction;
        }

        return result;
    })
    .DisableAntiforgery();

app.MapPost("/solve-sudoku", (ushort?[] numbers) =>
{
    if (numbers.Length != 81)
    {
        return Results.BadRequest("Please provide 9 numbers");
    }

    var sudokuInput = new SudokuInput
    {
        KnownValues =
        [
            [..GetRow(0)],
            [..GetRow(1)],
            [..GetRow(2)],
            [..GetRow(3)],
            [..GetRow(4)],
            [..GetRow(5)],
            [..GetRow(6)],
            [..GetRow(7)],
            [..GetRow(8)]
        ]
    };
    var solver = new SolveSudokuHandler(sudokuInput);

    var result = solver.Solve();
    return Results.Ok(result);

    ushort?[] GetRow(int rowIndex) => numbers[(rowIndex * 9)..(rowIndex * 9 + 9)];
});

app.Run();