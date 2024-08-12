# MTG Proxy Desk

**Make streaming your MTG games via webcam easier by streaming this application instead.**

Uses the **Scryfall API** to search for cards. Note that this is ==**NOT**== a full implementation of the game and it's many rules, but rather meant to emulate webcam streaming games with existing decks. It contains minimal checks, mostly for QoL.

Generates the folder **MTG_ProxyDesk** in **MyDocuments** on launch if missing, intended to house decks created with the application.

If you have an existing list of the names of cards in your deck in the format that you would feed into [Commander Deck Power Level Calculator](https://mtg.cardsrealm.com/en-us/tools/commander-power-level-calculator), there is a **Python** script generated in the **MyDocuments/MTG_ProxyDesk** folder when it's created which will convert that into a file usable by this application. It's recommended that you run it via the batch file so it can run various checks. There is a **README** generated with it in that folder which goes into further detail.

---

## MTG Proxy Deck File Format
- Byte #<sup>1</sup> => `bool` Commander Deck flag
- Byte #<sup>2</sup> => `int8` Number of cards in deck

**Loop:**
- Byte N<sup>0</sup> -> N<sup>31</sup> => `string` Scryfall Id <sub>(sans hyphens)</sub>
- Byte N<sup>32</sup> => `int8` Amount of this card in the deck
- Byte N<sup>33</sup> => `bool` No card limit flag <sub>(**Basic lands** or has text *"A deck can have any number of cards named **X**"*)</sub>
- Byte N<sup>34</sup> -> N<sup>37</sup> => `int32` Image byte length
- Byte N<sup>38</sup> -> N<sup>38 + Image Byte Length</sup> => `png | jpg` Image

---

## Current Progress
- Edit page can now load and save decks
- Play page not yet created