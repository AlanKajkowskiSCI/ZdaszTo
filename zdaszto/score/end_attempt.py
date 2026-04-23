from werkzeug.http import quote_etag

from zdaszto.misc import token_test


def end_attempt(request, cursor, connection):
    post_vars = request
    answers = post_vars.get('answers',None)
    if answers:
        try:
            answers2 = []
            user_id = token_test(request, cursor, connection).split(' ')[2]
            for pair in answers.split(";"):
                if not pair:
                    continue

                question_id_str, value_str = pair.split(":")
                question_id = int(question_id_str)
                value = int(value_str)

                if not isinstance(question_id, int) or question_id <= 0:
                    raise ValueError("question_id must be int and greater than 0")
                if not isinstance(value, int) or value not in (0, 1):
                    raise ValueError("value must be int and 0 or 1")

                print(question_id, value)
                answers2.append( (question_id, value) )

            values = []
            for question_id, correct in answers2:
                good = correct
                bad = (1 - correct)
                values.append( (question_id, user_id, 1, good, bad) )

            placeholders = ", ".join(["(%s, %s, %s, %s, %s)"] * len(values))
            values2 = []

            for _q in values:
                for _val in _q:
                    values2.append(_val)

            sql = f"""INSERT INTO `question_stats`(`id_question`, `id_user`, `times_solved`, `good_answers`, `wrong_answers`)
            VALUES {placeholders}
            ON DUPLICATE KEY UPDATE
            times_solved = times_solved + VALUES(times_solved),
            good_answers = good_answers + VALUES(good_answers),
            wrong_answers = wrong_answers + VALUES(wrong_answers);"""
            cursor.execute(sql, values2)
            connection.commit()
            response = "Results saved"
        except:
            response = "Invalid answers"
    else:
        response ="Invalid answers"

    return response
