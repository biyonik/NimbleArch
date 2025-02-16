using Microsoft.EntityFrameworkCore;

namespace NimbleArch.Infrastructure.Data.Exceptions;

public class DataAccessException: Exception
{
    public DataAccessException(string s, DbUpdateException ex): base(s, ex)
    {
        
    }

    public DataAccessException(string s, Exception exception): base(s, exception)
    {
    }
}