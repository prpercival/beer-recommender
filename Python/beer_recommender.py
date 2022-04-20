import time
import math
import spacy
import json
from statistics import mean
from pathlib import Path
from flask import Flask, request
from flask_cors import CORS
import logging

nlp = spacy.load("en_core_web_lg")  # make sure to use larger package!

numberOfBeersToLoad = 500


class Result:
    def __init__(self, name, similarity, link, brewery, style, alcohol, score):
        self.name = name
        self.similarity = similarity
        self.link = link
        self.brewery = brewery
        self.style = style
        self.alcohol = alcohol
        self.score = score

    def serialize(self):
        return {
            "name": self.name,
            "similarity": self.similarity,
            "link": self.link,
            "brewery": self.brewery,
            "style": self.style,
            "alcohol": self.alcohol,
            "score": self.score,
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

    for x in data:
        if beerNumber > numberOfBeersToLoad:
            break

        similarities = []

        for comment in x["nlp"]:
            similarity = input.similarity(comment)
            similarities.append(similarity)

        results.append(Result(x["Name"], mean(similarities), x["Link"], x["Brewery"], x["Style"], x["Alcohol"], x["Score"]))
        beerNumber = beerNumber + 1

    results.sort(key=lambda x: x.similarity, reverse=True)

    logging.info("--- %s seconds for recommendation ---" % (time.time() - start_time))

    return results


def init():
    print("Intializing data...")
    logging.info("Intializing data...")

    base_path = Path(__file__).parent
    f = open(f"{base_path}/data/beer_data_19_04_2022_05_45_25.json", encoding="utf-8")
    data = json.load(f)

    start_time = time.time()

    beerNumber = 1

    for x in data:
        if beerNumber > numberOfBeersToLoad:
            break

        logging.info(f'Intializing #{beerNumber}: {x["Name"]}')
        nlp_comments = []

        for comment in x["Comments"]:
            nlp_comments.append(nlp(comment))

        x["nlp"] = nlp_comments
        beerNumber = beerNumber + 1

    print("--- %s seconds to load ---" % (time.time() - start_time))
    logging.info("--- %s seconds to load ---" % (time.time() - start_time))
    return data


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
    logging.info("Starting server...")
    app.run(debug=False, host="0.0.0.0", port=5002)

# while True:
#     print("Please enter a description: ")
#     inp = input()
#     if inp == "exit":
#         break
#     else:
#         recommender(inp, data)
