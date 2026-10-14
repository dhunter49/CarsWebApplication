using Cars.Domain;
using Cars.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cars.API.Controllers;

public class CarsController : BaseApiController
{
    private readonly DataContext _context;
    
    public CarsController(DataContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Car>>> GetCars()
    {
        return await _context.Cars.AsNoTracking().ToListAsync();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Car>> GetCar(Guid id)
    {
        var car = await _context.Cars.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (car is null) return NotFound();
        return car;
    }
}