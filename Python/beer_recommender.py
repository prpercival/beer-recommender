import time
import math
import spacy
import json
from statistics import mean
from pathlib import Path
from flask import Flask, request
from flask_cors import CORS
from spacytextblob.spacytextblob import SpacyTextBlob

nlp = spacy.load("en_core_web_lg")  # make sure to use larger package!
nlp.add_pipe("spacytextblob")

numberOfBeersToLoad = 500


class Result:
    def __init__(self, name, recommendation, link, brewery, style, alcohol, score):
        self.name = name
        self.recommendation = recommendation
        self.link = link
        self.brewery = brewery
        self.style = style
        self.alcohol = alcohol
        self.score = score

    def serialize(self):
        return {
            "name": self.name,
            "recommendation": self.recommendation,
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

        recommendations = []

        for comment in x["nlp"]:
            similarity = input.similarity(comment)
            sentiment = comment._.blob.polarity
            recommendations.append(similarity + sentiment)

        results.append(
            Result(
                x["Name"],
                mean(recommendations),
                x["Link"],
                x["Brewery"],
                x["Style"],
                x["Alcohol"],
                x["Score"],
            )
        )
        beerNumber = beerNumber + 1

    results.sort(key=lambda x: x.recommendation, reverse=True)

    print("--- %s seconds for recommendation ---" % (time.time() - start_time))

    return results


def init():
    print("Intializing data...")

    base_path = Path(__file__).parent
    f = open(f"{base_path}/data/beer_data_19_04_2022_11_41_19.json", encoding="utf-8")
    data = json.load(f)

    start_time = time.time()

    beerNumber = 1

    for x in data:
        if beerNumber > numberOfBeersToLoad:
            break

        print(f'Intializing #{beerNumber}: {x["Name"]}')
        nlp_comments = []

        for comment in x["Comments"]:
            nlp_comments.append(nlp(comment))

        x["nlp"] = nlp_comments
        beerNumber = beerNumber + 1

    print("--- %s seconds to load ---" % (time.time() - start_time))
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
    app.run(debug=False, host="0.0.0.0", port=5002)   

# while True:
#     print("Please enter a description: ")
#     inp = input()
#     if inp == "exit":
#         break
#     else:
#         recommender(inp, data)
