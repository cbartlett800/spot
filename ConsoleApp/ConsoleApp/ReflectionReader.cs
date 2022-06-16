using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace ConsoleApp
{
    public class ReflectionReader
    {
        public string FolderToLookIn;

        const string indent = "   ";
        public void WriteAssemlyInfo(Assembly a)
        {

            FolderToLookIn = Path.GetDirectoryName(a.Location);
            StringBuilder s = new StringBuilder();

            var classTypes = a.DefinedTypes;

            foreach (var type in classTypes)
            {
                if (!type.Name.StartsWith("<"))
                    LogClass(type, s);
            }

            LogReferences(a, s);

            WriteFile(s);
        }


        private void WriteFile(StringBuilder s)
        {
            var fileName = Directory.GetCurrentDirectory() + @"\assemblylist.txt";

            if(System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);

            System.IO.File.WriteAllText(fileName, s.ToString());
        }

        private List<string> dependanciesAllreadyRead { get; set; }
        private void LogReferences(Assembly assembly, StringBuilder s)
        {
            s.AppendLine();
            s.AppendLine("REFERENCES: ");
            var depedependencies = new List<string>();
            var references = assembly.GetReferencedAssemblies().ToList();
            dependanciesAllreadyRead = new List<string>();
            foreach (var r in references)
            {
                //only add references here
                s.AppendLine(indent + r.FullName);
                //commenting this out because getting into the core .net assemblies
                //BuildDependancyTree(r, depedependencies);
            }

            s.AppendLine("Depedependencies: ");
            foreach(var dep in depedependencies)
            {
                s.AppendLine(indent + dep);
            }
        }
        private void BuildDependancyTree(AssemblyName r, List<string> depedependencies)
        {
            Assembly a = null;
            //see if the assembly is already loaded
            try
            {
                //a = Assembly.LoadFile(FolderToLookIn);
                var name = r.Name;
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                bool assembleLoaded = false;
                foreach (var loadedAssembly in loadedAssemblies)
                    if (loadedAssembly.GetName().Name == name)
                    {
                        a = loadedAssembly;
                        assembleLoaded = true;
                    }
                if(!assembleLoaded)
                    a = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);
            }
            catch (StackOverflowException ex)
            {
                //could not find assembly so this happens.
            }

            if (a == null)
            {
                try
                {
                    var path = String.Format("{0}{1}{2}{3}", FolderToLookIn, @"\", r.Name, ".dll");
                    if(System.IO.File.Exists(path))
                        a = Assembly.LoadFile(path);
                }
                catch
                {
                    //still could not load assembly with the full path of the current directory that the original dll was in
                }
            }
            if (a == null)
            {
                try
                {
                    a = Assembly.Load(r.FullName);
                }
                catch
                {
                    //could not load it via the gac
                }
            }
            if(!depedependencies.Contains(a.FullName))
                depedependencies.Add(a.FullName);
            if (a != null && !dependanciesAllreadyRead.Contains(a.FullName.ToLower()))
            {
                var subDepedependencies = a.GetReferencedAssemblies();

                foreach (var dependency in subDepedependencies)
                    BuildDependancyTree(dependency, depedependencies);
                dependanciesAllreadyRead.Add(a.FullName.ToLower());
            }
        }

        private void LogClass(TypeInfo type, StringBuilder s)
        {
            var className = type.Name;
            if (!className.StartsWith("<"))
            {
                s.Append(type.FullName);
                LogProperties(type, s);

                var gpConstraintsList = new List<string>();

                foreach (var parm in type.GetGenericArguments())
                    LogGenericConstraints(gpConstraintsList, parm, type, s);
            }
        }

        private void LogProperties(TypeInfo type, StringBuilder s)
        {
            foreach (var p in type.GetProperties())
            {
                string setVisibility = string.Empty;
                if (p.SetMethod != null)
                    setVisibility = GetVisibility(p.SetMethod);
                var getSet = "{0}{1}";
                if (p.CanRead)
                    getSet = String.Format("{0}{1}", "get;", p.CanWrite ? "{0}set;" : String.Empty);

                if (p.CanWrite && !string.IsNullOrEmpty(setVisibility) && setVisibility != "public")
                    getSet = String.Format(getSet, " " + setVisibility + " ");
                else if (setVisibility == "public")
                    getSet = getSet.Replace("{0}", string.Empty);


                getSet = "{ " + getSet + " }";
                string get = String.Empty;
                if (p.GetMethod != null)
                    get = GetVisibility(p.GetMethod);

                string propertyType = "object";
                try
                {
                    if (p.PropertyType != null)
                        propertyType = p.PropertyType.ToString();
                }
                catch
                {
                    //move on, sometimes we cannot load the dependancy and it throws an IO error.  Not much we can do about this
                }
                var line = string.Format("{0}{1} {2} {3} {4}"
                    , indent
                    , get
                    , propertyType
                    , p.Name
                    , getSet);

                if (line != string.Empty)
                    s.AppendLine(line);
            }
        }

        private void LogGenericConstraints(List<string> gpConstraintsList, Type parm, TypeInfo type, StringBuilder s)
        {
            var gpConstraints = parm.GetGenericParameterConstraints();

            foreach (var gp in gpConstraints)
                gpConstraintsList.Add(gp.Name);
            var header = indent + "Generic Constraints for " + type.Name + ":";
            s.Append(header);
            Console.WriteLine(header);
            foreach (var gpConstraint in gpConstraintsList)
            {
                var line = indent + indent + gpConstraint;
                s.Append(line);
                Console.WriteLine(line);
            }

        }
        public String GetVisibility(MethodInfo accessor)
        {
            if (accessor == null)
                return String.Empty;
            if (accessor.IsPublic)
                return "public";
            else if (accessor.IsPrivate)
                return "private";
            else if (accessor.IsFamily)
                return "protected";
            else if (accessor.IsAssembly)
                return "internal";
            else
                return "protected internal";
        }
        public bool CheckIfFileExists(string filepath)
        {
            return System.IO.File.Exists(filepath);
        }
    }
}
