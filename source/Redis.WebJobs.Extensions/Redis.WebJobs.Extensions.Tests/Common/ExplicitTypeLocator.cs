﻿using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Redis.WebJobs.Extensions.Tests.Common
{
    // Taken from WebJobs.Extentions.Tests on Github: // todo add url

    public class ExplicitTypeLocator : ITypeLocator
    {
        private readonly IReadOnlyList<Type> types;

        public ExplicitTypeLocator(params Type[] types)
        {
            this.types = types.ToList().AsReadOnly();
        }

        public IReadOnlyList<Type> GetTypes()
        {
            return types;
        }
    }
}
