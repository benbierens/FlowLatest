namespace FlowLatest
{
    public class Flow
    {
        private readonly string root;
        private readonly Action<string> log;
        private readonly List<FileOffset> files = new List<FileOffset>();
        private FileOffset? current;
        private FileInfo? candidate;

        public Flow(string root, Action<string> log)
        {
            this.root = root;
            this.log = log;
        }

        public void Run()
        {
            log("Flow: " + root);

            while (true)
            {
                Check();
                Thread.Sleep(300);
            }
        }

        private void Check()
        {
            TraversePath(root);
            ProcessCandidate();
        }

        private void TraversePath(string dir)
        {
            var dirs = Directory.GetDirectories(dir);
            foreach (var d in dirs) TraversePath(d);

            var files = Directory.GetFiles(dir);
            foreach (var f in files) TraverseFile(f);
        }

        private void TraverseFile(string f)
        {
            try
            {
                var info = new FileInfo(f);
                if (info == null || !info.Exists) return;
                
                if (candidate == null)
                {
                    candidate = info;
                    return;
                }

                if (info.LastWriteTimeUtc > candidate.LastWriteTimeUtc)
                {
                    candidate = info;
                }
            }
            catch { }
        }

        private void ProcessCandidate()
        {
            if (candidate == null) return;
            if (current == null)
            {
                current = new FileOffset(candidate);
                files.Add(current);
                return;
            }
            if (current.Info.FullName == candidate.FullName)
            {
                RollCurrent();
            }
            else
            {
                RollCurrent();
                log(Environment.NewLine + "Switch: " + candidate.FullName + Environment.NewLine);

                var existing = files.SingleOrDefault(f => f.Info.FullName == candidate.FullName);
                if (existing == null)
                {
                    current = new FileOffset(candidate);
                    files.Add(current);
                }
                else
                {
                    current = existing;
                }

                RollCurrent();
            }
        }

        private void RollCurrent()
        {
            if (current == null) throw new Exception("What?!");

            try
            {
                var lines = File.ReadAllLines(current.Info.FullName)
                    .Skip(current.LineOffset)
                    .ToArray();

                current.LineOffset += lines.Length;
                foreach (var line in lines) log("\t" + line);
            }
            catch { }
        }
    }

    public class FileOffset
    {
        public FileOffset(FileInfo info)
        {
            Info = info;
        }

        public FileInfo Info { get; }
        public int LineOffset { get; set; }
    }
}
