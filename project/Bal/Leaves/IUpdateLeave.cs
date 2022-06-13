using Dal.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public interface IUpdateLeave
    {
        void updateleave([FromBody] leave change);
    }
}
