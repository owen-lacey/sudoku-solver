using SudokuSolver.Api.Solver;
using SudokuSolver.Api.Solver.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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