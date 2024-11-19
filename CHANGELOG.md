Change Log
===========

#### v2.3.8
* All methods return an empty response if the symbol is not found.
* Bug fixes:
    * #9 QuerySummaryAsync return empty ExpandoObject for missed value
    * #10 Cache.GetTimeZone throws an exception if historical data is missing

#### v2.3.7
* Bug fixes:
  * #7 QuerySummaryAsync throw KeyNotFoundException

#### v2.3.6
* Search for security API
* Update summary results parsing

#### v2.3.5
* Request summary for different modules

#### v2.3
* Change the API to retrieve historical data.
* Add support for the portfolio API.
* Fix how the library handles different time zones.

#### v2.2
* Changed approach for getting auth cookie
* Added cookie authorization to all v7 APIs
* Upgraded all Nuget packages to most recent version and updated all code to work
* Updated tests project to .NET 6

* Added QueryAsync as a replacement of the original GetAsync method
* Added Fields for QueryAsync method

#### v2.1
* GetAsync method is obsoleted since Yahoo has terminated their csv quote service
* Added QueryAsync as a replacement of the original GetAsync method
* Added Fields for QueryAsync method

#### v2.0
* Removed error-proned timezone support
* All api call now reads and returns datetime in EST instead of local timezone.
* Removed ascending, leaveZeroWhenInvalid parameter in historical api call.
* IgnoreEmptyRows property in replacement with the original leaveZeroIfInvalid parameter.
* Performance boost on async calls.