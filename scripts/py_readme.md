## MTG Proxy Desk - Deck Converter

This converts the list that you would input into [Commander Deck Power Level Calculator](https://mtg.cardsrealm.com/en-us/tools/commander-power-level-calculator) into the JSON document that's read by the MTG Proxy Desk program. You can use this to create a deck instead of doing to via the UI. Running the 'bat' file is easier because you can just click on it, and it'll make sure you have **Python** and **PIP** installed. 

If you don't have **Python 3**, you'll need to go get it [Here.](https://apps.microsoft.com/detail/9nrwmjp3717k?hl=en-us&gl=US)

### Steps
1. Click on **ConvertDeck.bat**
2. Select file with cards listed like so: `Count CardName` ex. `1 Hazel of the Rootbloom`
    - If you're converting a **Commander** deck, ensure that it's listed first.
3. Select **Yes** or **No** for **Commander Deck**
3. Wait as it fetches the information for each card.
4. Select file to save as.