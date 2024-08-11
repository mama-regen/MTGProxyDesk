import os, sys, subprocess, tempfile
from time import time, sleep
from json import loads as parse
from typing import Callable, Literal
from urllib import request

def ImportInstall(*args) -> None:
    for a in args:
        import_name: str = a
        install_name: str = a
        if not isinstance(a, str):
            import_name = a[0]
            install_name = a[1]
        try: __import__(import_name)
        except:
            print(f'"{import_name}" not installed. Attempting install.')
            try:
                subprocess.check_call([sys.executable, "-m", "pip", "install", install_name])
                __import__(import_name)
            except Exception as e:
                print(f'Failed to install "{import_name}"!')
                raise e

def IsNumber(s) -> bool:
    try: int(s)
    except: return False
    return True

def FirstImg(card) -> tuple[str, str]:
    for c in ["png", "large", "normal"]:
        result: str|None  = card["image_uris"][c]
        if result != None: return (result, card["id"]+(".png" if c == ".png" else ".jpg"))

if __name__ == "__main__":
    ImportInstall('tkinter', 'requests', 'typing')
    
    from tkinter import filedialog, messagebox
    import requests as req

    Here: str = os.path.dirname(os.path.abspath((__file__)))

    GetCard: Callable[[str], dict] = lambda name: parse(req.get(f"https://api.scryfall.com/cards/named?fuzzy={name.replace(' ', '+')}").text)

    file_path: str = filedialog.askopenfilename(initialdir=Here)
    temp_fldr: Callable[..., str] = lambda *p: os.path.join(tempfile.gettempdir(), "mtg_prox_desk", *p)
    is_commander: bool = messagebox.askyesno("Commander?", "Is this a commander deck?")
    content: str = open(file_path, 'r').read()
    card_list: list[str] = content.split('\n')
    not_found: list[str] = []
    cards: list[tuple[str, int]] = []

    for i, line in enumerate(card_list):
        now: int = round(time() * 1000)

        cnt: int = 1
        card_name: str = ""
        splt: list[str] = [x for x in line.split(' ') if x.strip != '']
        if (IsNumber(splt[0])):
            cnt = int(splt[0])
            card_name = ' '.join(splt[1:])
        else: card_name = ' '.join(splt)
        print(f'Searching for "{card_name}"')
        
        card_actual = None
        try: card_actual: dict = GetCard(card_name)
        except Exception as e:
            not_found.append(card_name)
            continue

        image: tuple[str, str] = FirstImg(card_actual)
        request.urlretrieve(image[0], temp_fldr(image[1]))
        cards.append([card_actual["id"], cnt])

        wait_for: float = 100 - ((time() * 1000) - now)
        if (wait_for > 0): sleep(wait_for/1000)

    if len(not_found) > 0:
        msg: str = "The following cards were not found:"
        for card in not_found: msg += "\n\t" + card
        messagebox.showerror("Some Cards Not Found!", msg)

    save_path: str = filedialog.asksaveasfilename(initialdir=Here, defaultextension=".mpd", filetypes=[("MTG Proxy Deck", "*.mpd"), ("All Files", "*.*")])
    if save_path[-4:] != ".mpd": save_path += ".mpd"

    print("Saving file...")

    byte: Callable[[int], bytes] = lambda n, l=1: n.to_bytes(l, byteorder="big")
    splint: Callable[[int, int], int] = lambda n, p: (n >> (p * 8)) & 255
    with open(save_path, 'wb') as writer:
        writer.write(byte(1) if is_commander else byte(0))
        writer.write(byte(len(cards)))

        for c, card in enumerate(cards):
            print(f"Converting card[{c}/{len(cards)}]")
            for b in [ord(ch) for ch in card[0].replace("-", "")]: writer.write(byte(b))
            writer.write(byte(card[1]))
            ext: Literal[".png", ".jpg"] = ".png" if os.path.exists(temp_fldr(card[0]+".png")) else ".jpg"
            imgSize: int = os.path.getsize(temp_fldr(card[0]+ext))
            writer.write(byte(imgSize, 4))
            writer.write(open(temp_fldr(card[0]+ext), "rb").read())

    try: os.rmdir(temp_fldr())
    except: 
        for file in os.listdir(temp_fldr()):
            if os.path.isfile(temp_fldr(file)): os.remove(temp_fldr(file))