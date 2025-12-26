import zipfile
import json

class EmptyProduct(Exception):
    pass

def extract_product(sample: json) -> json:
    return sample['products']

def extract_region(samble: json) -> str:
    return samble['region']

def extract_product_count(product: json) -> int:
    c: str = product['count']
    if c is None or len(c) == 0 or not c[0].isdigit():
        return 1

    c = c.replace(" ", "")
    c = c.split(",")
    return int(c[0])

def extract_product_name(product: json) -> str:
    full_name: str = product['name']
    pos: int = full_name.find('\n')
    if pos == -1:
        return full_name.strip()
    else:
        return full_name[:pos+1].strip()

class Matcher:
    def __init__(self, pattern: str):
        self.pattern = pattern

    def match(self, s: str) -> bool:
        return s.lower().find(self.pattern) != -1

product_services: list[str] = [
    "услуги",
    "ремонт",
    "обслужив",
    "настройка",
    "поддержка",
    "оказание услуг",
    "работы",
    "замена",
    "пуско",
    "аттестация",
    "чистка",
    "установка"
]
product_service_matchers: dict[str, Matcher] = {n: Matcher(n) for n in product_services}

product_sw: list[str] = [
    "программно",
    "право",
    "лицензия",
]
product_sw_matchers: dict[str, Matcher] = {n: Matcher(n) for n in product_sw}

PRODUCT_NAMES: dict[str, str] = {
    "программы": "ПО",
    "услуги": "услуги",
    "прочее": "прочее",
    "мыш": "периферия",
    "клавиатур": "периферия",
    "наушник": "периферия",
    "накопит": "накопители",
    "колон": "периферия",
    "системн": "системный блок",
    "картридж": "картриджи",
    "катридж": "картриджи",
    "тонер": "картриджи",
    "чернила": "картриджи",
    "камер": "камеры",
    "комплектующ": "комплектующие",
    "конференц": "конференц-система",
    "модуль": "комплектующие",
    "монитор": "периферия",
    "сервер": "сервер",
    "хаб": "периферия",
    "блок пит": "блоки питания",
    "блоки пит": "блоки питания",
    "бесперебойн": "ИБП",
    "ибп": "ИБП",
    "ноутбук": "ноутбук",
    "моноблок": "моноблок",
    "персональный компьютер": "системный блок",
    "компьютер": "системный блок",
    "компьютеров": "системный блок",
    "компьютерной техники": "системный блок",
    "рабочая станци": "системный блок",
    "рабочих станци": "системный блок",
    "рабочие станци": "системный блок",
    "эвм": "системный блок",
    " пк ": "системный блок",
    "мфу": "принтер",
    "принтер": "принтер",
    "запасн": "комплектующие",
    "акустич": "периферия",
    "коммутатор": "коммутатор",
    "жестки": "накопитель",
    "маршрутизатор": "коммутатор",
    "роутер": "коммутатор",
    "источник питания": "блок питания",
    "источники питания": "блок питания",
    "планшет": "планшет",
    "интерактивн": "интерактивные приборы",
    "сканер": "принтер",
    "сканнер": "принтер",
    "телевизор": "телевизор",
    "проектор": "проектор",
    "телефон": "телефон",
    "терминал": "терминал",
    "ssd": "накопитель",
    "hdd": "накопитель",
    "гарнитура": "периферия",
    "микрофон": "периферия",
    "материнск": "комплектующие",
    "процессор": "комплектующие",
    "оперативн": "комплектующие",
    "дисковый массив": "накопитель",
    "виртуальной": "VR",
    "vr ": "VR",
    "дополненной": "AR",
    "ar ": "AR",
    "автоматизированное рабочее место": "АРМ",
    "kvm": "периферия",
    "квм": "периферия",
    "устройства запоминающие": "накопитель",
    "жёсткий диск": "накопитель",
    "вентилятор": "комплектующие",
    "привод": "комплектующие",
    "кулер": "комплектующие",
    "ddr": "комплектующие",
    "озу": "комплектующие",
    "провод": "прочее",
    "кабель": "прочее",
    "охлажден": "комплектующие",
    "корпус": "комплектующие",
    "видеокарта": "комплектующие",
    "носитель информации": "накопитель",
    "память": "комплектующие",
    "сетевая карта": "комплектующие",
    "сетевое хранилище": "СХД",
    "система хранения данных": "СХД",
    "арм" : "АРМ",
    "комплект пк" : "АРМ",
    "комплект арм": "АРМ",
    "расходных материалов": "картриджи",
    "термопаста": "комплектующие",
    "флеш": "накопитель",
    "флэш": "накопитель",
    "flash": "накопитель",
    "запчасти": "комплектующие",
    "схд": "СХД",
    "комлектующие": "комплектующие",
    "носители": "накопитель",
    "многофункциональное устройство": "принтер",
    "факс": "принтер",
    "ПК в сборе": "системный блок",
    "модули памяти": "комплектующие",
    "cpu": "комплектующие"
}

product_matchers: dict[str, Matcher] = {n: Matcher(n) for n in PRODUCT_NAMES.keys()}

def match_name(matches: dict[str, int], s: str, count: int = 1):
    if s == "":
        raise EmptyProduct()

    for _, matcher in product_service_matchers.items():
        if matcher.match(s):
            matches["услуги"] += 1
            return

    for _, matcher in product_sw_matchers.items():
        if matcher.match(s):
            matches["ПО"] += 1
            return

    any_match: bool = False
    for pattern, matcher in product_matchers.items():
        if matcher.match(s):
            any_match = True
            matches[PRODUCT_NAMES[pattern]] += 1

    if not any_match:
        matches["прочее"] += 1

def read_zip(filename: str, task):
    i = 0
    with zipfile.ZipFile(filename, 'r') as zip_ref:
        for file_name in zip_ref.namelist():
            with zip_ref.open(file_name) as f:
                i += 1
                task.process_json(json.load(f))

if __name__ == "__main__":
    class TaskCountAll:
        def __init__(self):
            self.product_matches: dict[str, int] = {n: 0 for n in PRODUCT_NAMES.values()}

        def process_json(self, sample: json):
            try:
                for product in extract_product(sample):
                    p = extract_product_name(product)
                    c: int = extract_product_count(product)
                    match_name(self.product_matches, p, c)
            except KeyError:
                    pass
            except EmptyProduct:
                    pass
            except ValueError:
                pass

    taskCountAll = TaskCountAll()
    read_zip("../zakupki-hw-data.zip", taskCountAll)
    for name, count in sorted(taskCountAll.product_matches.items(), key=lambda x: x[1], reverse=True):
        print(name, count)
