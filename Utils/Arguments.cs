using System.Collections.Generic;
using System.Linq;

namespace CryptUtils.Utils
{
    public class Arguments
    {
        private readonly string[] args;
        private readonly List<string> _errors = new List<string>();
        private readonly List<(string option, string value)> _options = new List<(string option, string value)>();
        private ILookup<string, (string option, string value)> _optionsLookUp = null;

        public Arguments(string[] args)
        {
            this.args = args;
        }

        public void Process()
        {
            if (_ProcessCommand())
            {
                _ProcessOptions();
            }

            if (!_errors.Any())
            {
                _optionsLookUp = _options.ToLookup(a => a.option);
            }
        }

        private bool _ProcessCommand()
        {
            var commands = args.TakeWhile(a => !a.StartsWith("-")).ToList();

            if (commands.Count > 1)
            {
                _errors.Add("Invalid program arguments");
            }
            else
            {
                string command = commands.FirstOrDefault()?.Trim();
                command = (command == "encrypt" || command == "decrypt") ? command : "encrypt";
                this.Command = command;
            }

            return _errors.Count == 0;
        }

        private bool _ProcessOptions()
        {
            var _opts = args.SkipWhile(a => !a.StartsWith("-")).Select(a => a.Trim()).ToArray();

            //args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();

            int i = 0;

            while (i < _opts.Length)
            {
                string opt = _opts[i].Trim();

                if (opt.StartsWith("-"))
                {
                    string val = _opts.ElementAtOrDefault(i + 1)?.Trim();

                    if (string.IsNullOrEmpty(val) || val.StartsWith("-"))
                    {
                        val = "";
                    }

                    if (!string.IsNullOrEmpty(val))
                    {
                        ++i;
                    }

                    _options.Add((opt, val));

                    ++i;
                }
                else
                {
                    _errors.Add("Invalid program arguments");
                    break;
                }
            }

            return _errors.Count == 0;
        }

        public IReadOnlyList<string> Errors
        {
            get { return _errors; }
        }

        public string Command
        {
            get;
            private set;
        }

        public IReadOnlyList<(string option, string value)> Options
        {
            get { return _options; }
        }

        public string GetOption(params string[] option)
        {
            var result = option.SelectMany(o => _optionsLookUp[o]).ToArray();
            return result.Length == 0 ? null : result[0].value;
        }

        public bool CheckValidOptions(IEnumerable<string> availableOptions)
        {
            bool isOk = true;

            foreach(var opt in _options)
            {
                if (!availableOptions.Contains(opt.option))
                {
                    _errors.Add($"Unrecognized option: {opt.option}");
                    isOk = false;
                }
            }

            return isOk;
        }
    }
}
