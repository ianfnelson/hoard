using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Instruments;                    
                                                                   
public record GetInstrumentsLookupQuery : IQuery<List<LookupDto>>;                                         
                                                                   
public class GetInstrumentsLookupQueryHandler(HoardContext context)                                                         
    : IQueryHandler<GetInstrumentsLookupQuery, List<LookupDto>>  
{                                                                
    public async Task<List<LookupDto>> HandleAsync(GetInstrumentsLookupQuery request, CancellationToken ct)  
    {                                                            
        return await context.Instruments        
            .AsNoTracking()
            .Where(i => i.InstrumentTypeId != InstrumentType.Cash)
            .OrderBy(i => i.Name)                                
            .Select(i => new LookupDto(                          
                i.Id,                                            
                i.Name,                                          
                i.TickerDisplay                                                        
            )) 
            .ToListAsync(ct);                                    
    }
}