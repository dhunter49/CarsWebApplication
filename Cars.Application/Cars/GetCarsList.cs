using Cars.Application.Cars.DTOs;
using Cars.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cars.Application.Cars;

public class GetCarsList
{
    public class Query : IRequest<List<CarDto>> { }

    public class Handler : IRequestHandler<Query, List<CarDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<CarDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Cars
                .AsNoTracking()
                .Select(car => new CarDto
                {
                    Id = car.Id,
                    Brand = car.Brand,
                    Model = car.Model,
                    DoorsNumber = car.DoorsNumber,
                    LuggageCapacity = car.LuggageCapacity,
                    EngineCapacity = car.EngineCapacity,
                    FuelType = car.FuelType,
                    ProductionDate = car.ProductionDate,
                    AverageFuelConsumption = car.AverageFuelConsumption,
                    BodyType = car.BodyType,
                    Photos = car.Photos
                        .Select(photo => new CarPhotoDto(photo.Id, photo.Url, photo.IsMain))
                        .ToList(),
                    Features = car.Features
                        .Select(feature => new FeatureDto(feature.Id, feature.Name))
                        .ToList(),
                })
                .ToListAsync(cancellationToken);
        }
    }
}