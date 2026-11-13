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
        private static MiningMetallurgyPlantEntities1 _context;

        public static MiningMetallurgyPlantEntities1 GetContext()
        {
            if (_context == null)
            {
                _context = new MiningMetallurgyPlantEntities1();
            }
            return _context;
        }
    }
}
