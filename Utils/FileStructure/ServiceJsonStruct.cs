
using System.Collections.Generic;

public class Rootobject
{
    public Solution Solution { get; set; }
    public Services Services { get; set; }
}

public class Solution
{
    public string ShortName { get; set; }
    public string FullName { get; set; }
    public Nuget Nuget { get; set; }
    public Ciserver CIServer { get; set; }
    public Production Production { get; set; }
}

public class Nuget
{
    public string Url { get; set; }
    public string ApiKey { get; set; }
}

public class Ciserver
{
    public string Type { get; set; }
    public string Url { get; set; }
}

public class Production
{
    public string Domain { get; set; }
}

public class Services
{
    public Dictionary<string , Service> All { get; set; }
}

public class Service
{
    public string LiveUrl { get; set; }
    public string UatUrl { get; set; }
}
