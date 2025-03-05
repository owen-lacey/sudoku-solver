using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using SudokuSolver_Api;
using SudokuSolver.Api.Detector;
using SudokuSolver.Api.Solver;
using SudokuSolver.Api.Solver.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPredictionEnginePool<SudokuDetector.ModelInput, SudokuDetector.ModelOutput>()
    .FromFile("SudokuDetector.mlnet");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Define prediction route & handler
app.MapPost("/detect",
    async (PredictionEnginePool<SudokuDetector.ModelInput, SudokuDetector.ModelOutput> predictionEnginePool, IFormFile uploadedImage) =>
    {
        var image = MLImage.CreateFromStream(uploadedImage.OpenReadStream());
        var input = new SudokuDetector.ModelInput
        {
            Image = image,
        };

        var modelOutput = predictionEnginePool.Predict(input);

        var bestScoreIndex = modelOutput.Score.ToList().IndexOf(modelOutput.Score.Max());

        var result = new DetectedSudoku
        {
            Score = modelOutput.Score[bestScoreIndex],
            Box = [
                modelOutput.PredictedBoundingBoxes[bestScoreIndex * 4],
                modelOutput.PredictedBoundingBoxes[bestScoreIndex * 4 + 1],
                modelOutput.PredictedBoundingBoxes[bestScoreIndex * 4 + 2],
                modelOutput.PredictedBoundingBoxes[bestScoreIndex * 4 + 3]
            ]
        };
        return await Task.FromResult(result);
    })
    .DisableAntiforgery();

app.MapPost("/solve", (ushort?[] numbers) =>
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