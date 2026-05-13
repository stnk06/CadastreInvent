using System;

namespace CadastreInvent.Shared.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Сущность \"{name}\" с идентификатором {key} не найдена.")
        {
        }
    }
}