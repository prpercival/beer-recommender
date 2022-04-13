import math
import spacy
import json
from statistics import mean
from pathlib import Path

nlp = spacy.load("en_core_web_lg")  # make sure to use larger package!

base_path = Path(__file__).parent
f = open(f'{base_path}\\data\\beer_data_06_04_2022_09_29_57.json')
data = json.load(f);

input = nlp("Malty, hoppy")

results = []

for x in data:
    similarities = []

    for comment in x.Comments:
        similarities.append(input.similarity(comment))

    results.append((x.Name, mean(similarities)))

results.sort(key=lambda x: x[1])