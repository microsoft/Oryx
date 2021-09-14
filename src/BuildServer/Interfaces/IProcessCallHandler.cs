// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Interfaces
{
    public interface IProcessCallHandler
    {
        void Execute(Func<IBuildServerAction, Task> buildServerTask);
    }
}
