using System.Collections.Generic;
using NativeUI;

namespace PhatCam
{
    public class CustomListItem : UIMenuListItem
    {
        public CustomListItem(string text, List<dynamic> items, int index) : base(text, items, index)
        {
        }

        public CustomListItem(string text, List<dynamic> items, int index, string description) : base(text, items, index, description)
        {
        }

        public override int ItemToIndex(dynamic item)
        {
            return _items.IndexOf(item);
        }

        public override dynamic IndexToItem(int index)
        {
            return _items[index];
        }
    }
}
