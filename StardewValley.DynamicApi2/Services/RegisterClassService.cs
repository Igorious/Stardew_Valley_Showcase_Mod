using System;
using System.Collections.Generic;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public class RegisterClassService
    {
        public static RegisterClassService Instance { get; } = new RegisterClassService();

        private IDictionary<int, Type> FurnitureMappings { get; } = new Dictionary<int, Type>();

        public void Furniture<T>(int id) where T : Furniture
        {
            FurnitureMappings.Add(id, typeof(T));
        }

        public Type TryGetFurniture(int id)
        {
            return FurnitureMappings.TryGetValue(id, out Type type)? type : null;
        }

        public Type TryGet(Object o)
        {
            switch (o)
            {
                case Furniture furniture:
                    return TryGetFurniture(o.ParentSheetIndex);
                default:
                    return null;
            }
        }
    }
}