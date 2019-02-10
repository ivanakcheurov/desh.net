# desh.net

| Appveyor | NuGet | License |
|----------|-------| --------|
|[![Build status](https://ci.appveyor.com/api/projects/status/github/ivanakcheurov/desh.net?svg=true)](https://ci.appveyor.com/project/ivanakcheurov/desh-net/branch/master) |[![NuGet](https://img.shields.io/nuget/v/desh.svg)](https://www.nuget.org/packages/desh/) [![Usage](https://img.shields.io/nuget/dt/desh.svg)](https://www.nuget.org/stats/packages/desh?groupby=Version) |[![License](https://img.shields.io/github/license/ivanakcheurov/desh.net.svg)](https://github.com/ivanakcheurov/desh.net/blob/master/license.MIT)|

## Why Desh?
Desh is a concise and readable language to describe business rules. 
- The language, parser and execution engine are generic and fit for any business domain.
- The language is based on YAML, in other words, any Desh document is a valid YAML file.
## How to use
Just install the [Desh NuGet package](https://www.nuget.org/packages/desh/):
```
PM> Install-Package YamlDotNet
```
Craft a Desh document (YAML, an example below).
Let's take an example of an online travel agency. Customers want to know if they need a visa when traveling to another country.

The following Desh document describes business rules if a person needs a visa:
```yaml
nationality:
  - NL:
      destination:
        - US: electronic_online_visa
        - UA:
            days_of_stay:
              - number_greater_than: 90
                decide: paper_visa
              - else: no_visa
  - UA:
      destination:
        - US: paper_visa
        - NL:
            biometric_passport:
              - yes: no_visa
              - no: paper_visa
```
Then we need customer's data:
```yaml
nationality: NL
destination: US
```
Then we can feed this to Desh engine.
```csharp
var desh = @"
nationality:
  - NL:
      destination:
        - US: electronic_online_visa
        - UA:
            days_of_stay:
              - number_greater_than: 90
                decide: paper_visa
              - else: no_visa
  - UA:
      destination:
        - US: paper_visa
        - NL:
            biometric_passport:
              - yes: no_visa
              - no: paper_visa
";
var customer =
  new Dictionary<string, Func<string>>
  {
    { "nationality", () => "NL" }, 
    { "destination", () => "US" }
  };
var facade = new DeshFacade();
var visaDecision = facade.ParseAndMakeStringDecision(desh, customer);
Console.WriteLine($"Visa requirement: {visaDecision}");
// outputs "Visa requirement: electronic_online_visa"
```
### What does the name stand for?
Desh is a *phonetic* abbreviation of **D**ecision **E**xpre**ss**ions.
