# API-NG-Basic-WinForm-App
Betfair API-NG Simple Winform Demo. Created using Visual Studio 2022 Community Edition

Pretty much the same as the Betfair Console demo, but as a Winform App. with a couple of fixes and improvements.

Dont forget to add your Betfair App. Key in Connection.cs, else it wont work.
I havent tried it with a Betfair Delayed Key, but presumably should work.

Changes
-------
1) Fixed wrong TLS version in Betfair Console App.
2) Fixed Betfair Console App issue where every run it placed an unmatched 10 pence bet into the market.
3) Rejigged as a Winform App. with commands split off to separate buttons
4) Added login and logout functions so you dont need to faff around manually getting a session token
5) Added missing marketStartTime variable to a data structure, so that it can display race start time and venue now
6) Added some extra data print outs to text window for information purposes
