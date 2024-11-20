using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoManager.src
{
    class Details
    {

        private int _id;
        private string _name;
        private string _description;

        public Details(int id)
        {
            _id = id;
            _name = string.Empty;
            _description = string.Empty;
        }
        public Details(int id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
        }
        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }
}
