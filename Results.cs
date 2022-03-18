namespace Fnr
{
    abstract public class Results
    {
        public string File_name { get; set; }
        public string File_path { get; set; }
        public int Conformity { get; set; }
        public bool Success { get; set; }

        public bool Check_include
        {
            get
            {
                if (Success && Conformity > 0) return true;
                return false;
            }
        }
    }
}
