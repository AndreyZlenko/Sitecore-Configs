using System;
using System.ComponentModel.Design;

namespace BuildConfigTransformation
{
    public static class CommandList
    {
        public static CommandID PrewiewTransform = new CommandID(new Guid("9e2bd081-7249-4daf-885e-b1c010f16ad8"), 0x0100);
        public static CommandID AddTransform = new CommandID(new Guid("9e2bd081-7249-4daf-885e-b1c010f16ad8"), 0x0110);
        public static CommandID UpdateTransform = new CommandID(new Guid("9e2bd081-7249-4daf-885e-b1c010f16ad8"), 0x0120);
        public static CommandID RemoveTransform = new CommandID(new Guid("9e2bd081-7249-4daf-885e-b1c010f16ad8"), 0x0140);
    }
}
