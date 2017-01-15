using Igorious.StardewValley.DynamicApi2.Utils;
using StardewValley;
using StardewValley.Objects;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public class Wrapper
    {
        public static Wrapper Instance { get; } = new Wrapper();

        public Object Wrap(Object basicObject)
        {
            if (basicObject == null) return null;

            var customType = RegisterClassService.Instance.TryGet(basicObject);
            if (customType == null || basicObject.GetType() == customType) return basicObject;

            var customObject = new Constructor<int, Object>(customType).Invoke(basicObject.ParentSheetIndex);
            Cloner.Instance.CopyData(basicObject, customObject);
            return customObject;
        }

        public Object Unwrap(Object customObject)
        {
            if (customObject == null) return null;

            var customType = RegisterClassService.Instance.TryGet(customObject);
            if (customObject.GetType() != customType) return customObject;

            Object basicObject;
            switch (customObject)
            {
                case Furniture furniture:
                    basicObject = new Constructor<Furniture>().Invoke();
                    break;
                default:
                    basicObject = new Constructor<Object>().Invoke();
                    break;
            }

            Cloner.Instance.CopyData(customObject, basicObject);
            basicObject.heldObject = Unwrap(basicObject.heldObject);
            return basicObject;
        }
    }
}