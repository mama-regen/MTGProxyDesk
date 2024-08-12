# MTG Proxy Desk

**Make streaming your MTG games via webcam easier by streaming this application instead.**

Uses the **Scryfall API** to search for cards. Note that this is **NOT** a full implementation of the game and it's many rules, but rather meant to emulate webcam streaming games with existing decks. It contains minimal checks, mostly for QoL.

Generates the folder **MTG_ProxyDesk** in **MyDocuments** on launch if missing, intended to house decks created with the application.

If you have an existing list of the names of cards in your deck in the format that you would feed into [Commander Deck Power Level Calculator](https://mtg.cardsrealm.com/en-us/tools/commander-power-level-calculator), there is a **Python** script generated in the **MyDocuments/MTG_ProxyDesk** folder when it's created which will convert that into a file usable by this application. It's recommended that you run it via the batch file so it can run various checks. There is a **README** generated with it in that folder which goes into further detail.

## Current Progress
- The initial page and the edit page are created
- The edit page will load a deck via **Browse** and **Edit**
- The edit page allows for adding and removing cards via the controls but does not currently save.