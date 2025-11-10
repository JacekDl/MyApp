using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Domain.Common
{
    public record class HResult
    {
        public bool Succeeded => string.IsNullOrEmpty(ErrorMessage);
        public string? ErrorMessage { get; init; }
    }

    public record class HResult<TValue> : HResult
    {
        public TValue? Value { get; init; }
    }
}
