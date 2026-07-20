def fuse_results(
    disease_results,
    insect_results,
    growth_results
):

    result = {
        "plant": None,
        "disease": None,
        "insect": None,
        "growth_stage": None
    }

    if disease_results:

        top = disease_results[0]

        parts = top["class_name"].split("___")

        result["plant"] = parts[0]

        if len(parts) > 1:
            result["disease"] = parts[1]

    if insect_results:
        result["insect"] = insect_results[0]["class_name"]

    if growth_results:
        result["growth_stage"] = growth_results[0]["class_name"]

    return result