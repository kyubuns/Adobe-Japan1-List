#!/usr/bin/env dotnet-script

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

var lines = File.ReadAllText("generator/cid2code.txt").Split('\n');
var output = new (string, (int, int))[]
{
    ("Adobe-Japan1-1", (0, 8358)),
    ("Adobe-Japan1-2", (0, 8719)),
    ("Adobe-Japan1-3", (0, 9353)),
    ("Adobe-Japan1-4", (0, 15443)),
    ("Adobe-Japan1-5", (0, 20316)),
    ("Adobe-Japan1-6", (0, 23057)),
    ("Adobe-Japan1-7", (0, 23059)),
};

var header = new string[]{};
var data = new List<Dictionary<string, string>>{};

foreach (var line in lines)
{
    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

    var elements = line.Split('\t');
    if (elements[0] == "CID")
    {
        header = elements;
        continue;
    }
    var e = new Dictionary<string, string>();
    for (var i = 0; i < header.Length; ++i)
    {
        e[header[i]] = elements[i];
    }
    data.Add(e);
}

foreach (var (name, range) in output)
{
    var targets = data
        .Where(x => range.Item1 <= int.Parse(x["CID"]) && int.Parse(x["CID"]) <= range.Item2)
        .SelectMany(x =>
        {
            // Console.WriteLine($"{x["CID"]}, {x["UniJIS-UTF32"]}");
            return x["UniJIS-UTF32"].Split(",");
        })
        .Where(x => x != "*")
        .Select(x => x.Replace("v", ""))
        .Select(x =>
        {
            var bytes = new List<byte>();
            for (int i = 0; i < x.Length; i = i + 2)
            {
                bytes.Insert(0, Convert.ToByte(x.Substring(i, 2), 16));
            }
            return bytes.ToArray();
        })
        .Select(x =>
        {
            var a = Encoding.UTF32.GetString(x);
            // Console.WriteLine($" -> {a}");
            return a;
        })
        .ToArray();
    var rawText = string.Join("", targets);
    File.WriteAllText($"{name}.txt", rawText);
    Console.WriteLine($"- {name} {targets.Count()}字");
}
