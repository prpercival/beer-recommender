import time
import math
import spacy
import json
import re
import string
from statistics import mean
from pathlib import Path
from flask import Flask, request
from flask_cors import CORS
from spacytextblob.spacytextblob import SpacyTextBlob
import nltk
from nltk.corpus import stopwords

nlp = spacy.load("en_core_web_lg")  # make sure to use larger package!
nlp.add_pipe("spacytextblob")

nltk.download("stopwords")
stop = stopwords.words("english")

numberOfBeersToLoad = 500


class Result:
    def __init__(
        self, name, score, similarity, sentiment, link, brewery, style, alcohol, rating
    ):
        self.name = name
        self.score = score
        self.similarity = similarity
        self.sentiment = sentiment
        self.link = link
        self.brewery = brewery
        self.style = style
        self.alcohol = alcohol
        self.rating = rating

    def serialize(self):
        return {
            "name": self.name,
            "score": self.score,
            "similarity": self.similarity,
            "sentiment": self.sentiment,
            "link": self.link,
            "brewery": self.brewery,
            "style": self.style,
            "alcohol": self.alcohol,
            "rating": self.rating,
        }

    def toJSON(self):
        return json.dumps(
            self, default=lambda obj: obj.__dict__
        )  # , sort_keys=True, indent=4)


def recommender(input, data):
    start_time = time.time()
    results = []

    input = nlp(input)

    beerNumber = 1

    for beer in data:
        if beerNumber > numberOfBeersToLoad:
            break

        beerNlp = beer["nlp"]

        similarity = input.similarity(beerNlp)
        sentiment = beerNlp._.blob.polarity

        results.append(
            Result(
                beer["Name"],
                similarity,
                similarity,
                sentiment,
                beer["Link"],
                beer["Brewery"],
                beer["Style"],
                beer["Alcohol"],
                beer["Score"],
            )
        )
        beerNumber = beerNumber + 1

    results.sort(key=lambda x: x.score, reverse=True)

    print("--- %s seconds for recommendation ---" % (time.time() - start_time))

    return results


def init():

    print("Intializing data...")

    start_time = time.time()

    base_path = Path(__file__).parent
    f = open(f"{base_path}/data/beer_data_05_05_2022_09_51_45.json", encoding="utf-8")
    data = json.load(f)

    for beer in data:
        words = []

        for comment in beer["Comments"]:
            comment = re.sub(
                "[%s]" % re.escape(string.punctuation), "", comment.lower()
            )
            for word in comment.split():
                if word not in stop and len(word) != 1:
                    words.append(word)

        beer["Words"] = words

    beerNumber = 1

    for beer in data:
        # print(f'Intializing #{beerNumber}: {beer["Name"]}')

        #getFrequencies(beer)

        beer["nlp"] = nlp(' '.join(beer["Words"]))

        #print(f"#{beerNumber} {beer['Name']}:#{beer['Comparison']}")
        print(f"#{beerNumber} {beer['Name']}")

        beerNumber = beerNumber + 1

    print("--- %s seconds to load ---" % (time.time() - start_time))
    return data


def getFrequencies(beer):
    frequencies = {}

    for word in beer["Words"]:
        if frequencies.get(word):
            frequencies[word] += 1
        else:
            frequencies[word] = 1

    frequencies = {
        k: v
        for k, v in sorted(frequencies.items(), key=lambda item: item[1], reverse=True)
    }

    beer["Frequencies"] = frequencies
    beer["Comparison"] = ""
    index = 0

    for frequency in frequencies:
        index += 1

        if index > 25:
            break

        beer["Comparison"] += f" {frequency}"


app = Flask(__name__)
CORS(app)

spacyData = init()


@app.route("/recommender/recommend")
def endpoint():
    args = request.args

    recommendation = recommender(args.get("description"), spacyData)

    return json.dumps([ob.__dict__ for ob in recommendation])


if __name__ == "__main__":
    # run() method of Flask class runs the application
    # on the local development server.
    print("Starting server...")
    app.run(debug=False, host="0.0.0.0", port=5002)

# while True:
#     print("Please enter a description: ")
#     inp = input()
#     if inp == "exit":
#         break
#     else:
#         recommender(inp, data)
