using pr_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pr_3
{
    internal class Helper
    {
        private static MiningMetallurgyPlantEntities _context;

        public static MiningMetallurgyPlantEntities GetContext()
        {
            if (_context == null)
            {
                _context = new MiningMetallurgyPlantEntities();
            }
            return _context;
        }
    }
}
