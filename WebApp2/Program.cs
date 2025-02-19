using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

var connectionString = "Server=AMINAHM\\SQLEXPRESS;Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5041");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseCors("AllowAll");

app.MapGet("/izdelek", async (InventoryContext db) => 
    await db.Izdelek.FromSqlRaw("SELECT * FROM Izdelek").ToListAsync());

app.MapGet("/skladisce", async (InventoryContext db) => 
    await db.Skladisce.FromSqlRaw("SELECT * FROM Skladisce").ToListAsync());

app.MapGet("/skladisce-has-izdelek", async (InventoryContext db) =>
    await db.SkladisceHasIzdelek
        .Include(shi => shi.Izdelek)  
        .Include(shi => shi.Skladisce) 
        .Select(shi => new
        {
            idIz = shi.IDIz,
            idSk = shi.IDSk,
            IzdelekNaziv = shi.Izdelek.Naziv,  
            SkladisceNaziv = shi.Skladisce.Naziv, 
            Zaloga = shi.Zaloga
        })
        .ToListAsync()
);

app.MapGet("/izdelek-brez-skladisca", async (InventoryContext db) =>
    await db.Izdelek.FromSqlRaw(@"
        SELECT * FROM Izdelek 
        WHERE ID NOT IN (SELECT IDIz FROM SkladisceHasIzdelek)")
    .ToListAsync()
);


app.MapPut("/izdelek/{id}", async (InventoryContext db, int id, Izdelek updatedItem) =>
{
    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "UPDATE Izdelek SET Naziv = {0} WHERE ID = {1}", updatedItem.Naziv, id);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});

app.MapPut("/skladisce/{id}", async (InventoryContext db, int id, Skladisce updatedItem) =>
{
    Console.WriteLine($"Received PUT request: ID={id}, Naziv={updatedItem.Naziv}");

    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "UPDATE Skladisce SET Naziv = {0} WHERE ID = {1}", updatedItem.Naziv, id);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});

app.MapPut("/skladisce-has-izdelek/{idSk}/{idIz}/{zaloga}", async (InventoryContext db, int idSk, int idIz, int zaloga) =>
{
    Console.WriteLine($"Received PUT request: idSk={idSk}, idIz={idIz}, zaloga={zaloga}");
    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "UPDATE SkladisceHasIzdelek SET Zaloga = {0} WHERE IDSk = {1} AND IDIz = {2}", zaloga, idSk, idIz);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});


app.MapPost("/izdelek", async (InventoryContext db, Izdelek newItem) =>
{
    int affectedRows = await db.Database.ExecuteSqlAsync($@"
        INSERT INTO Izdelek (ID, Naziv) 
        VALUES (
            (SELECT MIN(t1.ID) + 1 
             FROM (SELECT 0 AS ID UNION ALL SELECT ID FROM Izdelek) t1 
             LEFT JOIN Izdelek t2 ON t1.ID + 1 = t2.ID
             WHERE t2.ID IS NULL), 
            {newItem.Naziv});"
    );

    return affectedRows > 0 ? Results.Ok(newItem) : Results.BadRequest();
});

app.MapPost("/skladisce", async (InventoryContext db, Skladisce newItem) =>
{
    int affectedRows = await db.Database.ExecuteSqlAsync($@"
        INSERT INTO Skladisce (ID, Naziv) 
        VALUES (
            (SELECT MIN(t1.ID) + 1 
             FROM (SELECT 0 AS ID UNION ALL SELECT ID FROM Skladisce) t1 
             LEFT JOIN Skladisce t2 ON t1.ID + 1 = t2.ID
             WHERE t2.ID IS NULL), 
            {newItem.Naziv});"
    );

    return affectedRows > 0 ? Results.Ok(newItem) : Results.BadRequest();
});



app.MapPost("/skladisce-has-izdelek", async (InventoryContext db, SkladisceHasIzdelek newItem) =>
{
    int affectedRows = await db.Database.ExecuteSqlAsync($@"
        INSERT INTO SkladisceHasIzdelek (IDSk, IDIz, Zaloga) 
        VALUES ({newItem.IDSk}, {newItem.IDIz}, {newItem.Zaloga});"
    );

    return affectedRows > 0 ? Results.Ok(newItem) : Results.BadRequest();
});






app.MapDelete("/izdelek/{id}", async (InventoryContext db, int id) =>
{
    await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM SkladisceHasIzdelek WHERE IDIz = {0}", id);

    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM Izdelek WHERE ID = {0}", id);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});

app.MapDelete("/skladisce/{id}", async (InventoryContext db, int id) =>
{
    await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM SkladisceHasIzdelek WHERE IDSk = {0}", id);

    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM Skladisce WHERE ID = {0}", id);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});


app.MapDelete("/skladisce-has-izdelek/{idSk}/{idIz}", async (InventoryContext db, int idSk, int idIz) =>
{
    int affectedRows = await db.Database.ExecuteSqlRawAsync(
        "DELETE FROM SkladisceHasIzdelek WHERE IDSk = {0} AND IDIz = {1}", idSk, idIz);
    
    return affectedRows > 0 ? Results.Ok() : Results.NotFound();
});

app.Run();
