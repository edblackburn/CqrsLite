﻿using System;

namespace CqrsLite.Framework.Domain.Exception
{
    public class ConcurrencyException : System.Exception
    {
        public ConcurrencyException(Guid id)
            : base($"A different version than expected was found in aggregate {id}.")
        {
        }
    }
}