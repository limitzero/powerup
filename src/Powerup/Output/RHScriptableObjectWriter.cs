using System;
using System.IO;
using Powerup.Model;

namespace Powerup.Output
{
    public class RHScriptableObjectWriter
    {
        private readonly IScriptableDatabaseObject _scriptableDatabaseObject;
        private readonly string _directory;

        public RHScriptableObjectWriter(IScriptableDatabaseObject scriptableDatabaseObject, string directory)
        {
            _scriptableDatabaseObject = scriptableDatabaseObject;
            _directory = directory;
        }

        public void WriteContent()
        {
            var fileName = string.Format("{0}.sql", _scriptableDatabaseObject.Name.Replace(" ", "_"));
            using ( var sw = File.CreateText(Path.Combine(_directory, fileName)) )
            {
                sw.Write(_scriptableDatabaseObject.Script());
                Console.WriteLine("Creating file '{0}' at [{1}]", fileName, _directory);
            }
        }
    }
}