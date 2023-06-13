using Microsoft.EntityFrameworkCore;
using MinimalAPI_Sample.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CourseContext>(options => options.UseInMemoryDatabase("CourseDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.MapGet("/course", async (CourseContext context, CancellationToken ct) =>
{
    var result = await context.Courses.ToListAsync(ct);
    return Results.Ok(result);
}).WithOpenApi();

app.MapPost("/course", async (CourseDto dto, CourseContext context, CancellationToken ct) =>
{
    var course = new Course
    {
        Name = dto.Name,
        Teacher = dto.Teacher
    };

    await context.Courses.AddAsync(course, ct);
    await context.SaveChangesAsync(ct);

    return Results.Created("/course/{courseId}", course);
}).WithOpenApi();

app.MapGet("/course/{courseId}", async (int id, CourseContext context, CancellationToken ct) =>
{
    var course = await context.Courses.FirstOrDefaultAsync(i => i.Id == id);
    if (course is null)
        return Results.NotFound();

    return Results.Ok(course);
}).WithOpenApi();

app.MapDelete("course/{courseId}", async (int id, CourseContext context, CancellationToken ct) =>
{
    var course = await context.Courses.FirstOrDefaultAsync(i => i.Id == id);
    if (course is null)
        return Results.NotFound();

    context.Courses.Remove(course);
    await context.SaveChangesAsync(ct);

    return Results.NoContent();

}).WithOpenApi();


app.Run();

#region Dto

public record CourseDto(string Name, string Teacher);

#endregion

#region Models

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Teacher { get; set; }
}


#endregion

#region Context

public class CourseContext : DbContext
{
    public CourseContext(DbContextOptions<CourseContext> options) : base(options) { }

    public DbSet<Course> Courses { get; set; }
}

#endregion