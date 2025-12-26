import pandas as pd
import geopandas as gpd
import matplotlib.pyplot as plt
import json

from common import *

class Task1:
    def __init__(self):
        self.regions: dict[str, dict[str, int]] = {}

    def process_json(self, sample: json):
        try:
            r = extract_region(sample)
            if self.regions.get(r) is None:
                 self.regions[r] = {n: 0 for n in PRODUCT_NAMES.values()}

            for product in extract_product(sample):
                try:
                    p: str = extract_product_name(product)
                    c: int = extract_product_count(product)
                    match_name(self.regions[r], p, c)
                except KeyError:
                        pass
                except EmptyProduct:
                        pass
                except ValueError:
                    pass
        except KeyError:
            pass

REGION_MAPPING: str = {
    "Байконур г": "",
    "Донецкая Народная Респ": "Донецкая Народная Республика",
    "Еврейская Аобл": "Еврейская автономная область",
    "Кабардино-Балкарская Респ": "Кабардино-Балкарская Республика",
    "Карачаево-Черкесская Респ": "Карачаево-Черкесская Республика",
    "Кемеровская областьасть - Кузбасс область": "Кемеровская область",
    "Луганская Народная Респ": "Луганская Народная Республика",
    "Ненецкий АО": "Ненецкий автономный округ",
    "Севастополь г": "г.Севастополь",
    "Северная Осетия - Алания Респ": "Республика Северная Осетия",
    "Удмуртская Респ": "Удмуртская Республика",
    "Ханты-Мансийский Автономный округ - Югра АО": "Ханты-Мансийский автономный округ",
    "Чеченская Респ": "Чеченская Республика",
    "Чувашская Республика - Чувашия": "Чувашская Республика",
    "Чукотский АО": "Чукотский автономный округ",
    "Ямало-Ненецкий АО": "Ямало-Ненецкий автономный округ",
    "Кемеровская область - Кузбасс обл": "Кемеровская область",
}

def transform_region(region: str) -> str:
    res: str = REGION_MAPPING.get(region)
    if res is not None:
        return res

    if region.find(" обл") != -1:
        return region.replace(" обл", " область")
    elif region.find(" Респ") != -1:
        return f"Республика {region[:region.find(" Респ")+1]}"
    return region

def geo_plot(data, title, legend_label, filename=None):
    df = pd.DataFrame(data, columns=('region', 'count'))
    merged_data = geo_data.merge(df, left_on='name', right_on='region', how='left')
    merged_data['count'] = merged_data['count'].fillna(0)
    merged_data.crs = None
    merged_data = merged_data.set_crs(epsg=3857)

    fig, ax = plt.subplots(1, 1, figsize=(16, 9))
    merged_data.plot(
        column='count',
        cmap='Blues',
        linewidth=0.8,
        edgecolor='0.8',
        legend=True,
        missing_kwds={'color': 'lightgrey'},
        ax=ax,
        legend_kwds={
            'label': legend_label,
            'orientation': 'vertical',
            'shrink': 0.7,
            'pad': -0.01,
        }
    )

    top10 = sorted(data, key=lambda x: x[1], reverse=True)[:10]
    top10_text = "Топ 10:\n" + "\n".join(f"{region:<35} {value:>5}" for region, value in top10)

    ax.text(
        x=0.02,
        y=0.92,
        s=top10_text,
        transform=ax.transAxes,
        fontsize=12,
        fontfamily='monospace',
        verticalalignment='top',
        bbox=dict(boxstyle="round,pad=0.5", facecolor="white", alpha=0.8)
    )

    ax.set_axis_off()
    plt.title(title, fontsize=15, fontweight='bold', y=0.96)
    plt.tight_layout()
    if filename is not None:
        plt.savefig(filename, dpi=300, bbox_inches='tight', pad_inches=0)
    else:
        plt.show()


if __name__ == "__main__":
    task1 = Task1()
    read_zip("../zakupki-hw-data.zip", task1)

    region_service = []
    region_pc = []
    region_arm = []
    region_storage = []
    region_ink = []

    for r, d in sorted(task1.regions.items(), key=lambda x: x[0]):
        tr = transform_region(r).strip()
        if tr == '': # no mapping
            continue
        region_service.append((tr, d.get("услуги", 0)))
        region_pc.append((tr, d.get("системный блок", 0)))
        region_arm.append((tr, d.get("АРМ", 0)))
        region_storage.append((tr, d.get("накопитель", 0)))
        region_ink.append((tr, d.get("картриджи", 0)))

    geo_data = gpd.read_file('rus_simple_highcharts.geo.json')

    geo_plot(region_service, "Распределение регионов по закупке услуг (ремонт и обслуживание)", "Количество услуг", "region-service-heatmap")
    geo_plot(region_pc, "Распределение регионов по закупке компьютеров", "Количество, шт", "region-pc-heatmap.png")
    geo_plot(region_arm, "Распределение регионов по закупке АРМ", "Количество, шт", "region-arm-heatmap.png")
    geo_plot(region_storage, "Распределение регионов по закупке накопителей", "Количество, шт", "region-storage-heatmap.png")
