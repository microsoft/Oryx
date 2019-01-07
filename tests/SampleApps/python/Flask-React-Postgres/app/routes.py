from flask import Flask
from flask import request, render_template, jsonify, g

from app.utils.auth import generate_token, requires_auth, verify_token
from app.models.models import User, Task
from app import app

import os


@app.route('/', methods=['GET'])
def index():
    return render_template('index.html')


@app.route('/<path:path>', methods=['GET'])
def any_root_path(path):
    return render_template('index.html')


@app.route("/api/user", methods=["GET"])
@requires_auth
def get_user():
    return jsonify(result=g.current_user,
                    tasks=Task.get_latest_tasks())


@app.route("/api/create_user", methods=["POST"])
def create_user():
    incoming = request.get_json()
    
    success = User.create_user(incoming)

    if not success:
        return jsonify(message="User with that email already exists"), 409

    new_user = User.query.filter_by(email=incoming["email"]).first()

    return jsonify(
        id=new_user.id,
        token=generate_token(new_user)
    )


@app.route("/api/get_token", methods=["POST"])
def get_token():
    incoming = request.get_json()
    user = User.get_user_with_email_and_password(incoming["email"], incoming["password"])
    if user:
        return jsonify(token=generate_token(user))

    return jsonify(error=True), 403


@app.route("/api/is_token_valid", methods=["POST"])
def is_token_valid():
    incoming = request.get_json()
    is_valid = verify_token(incoming["token"])

    if is_valid:
        return jsonify(token_is_valid=True)
    else:
        return jsonify(token_is_valid=False), 403


@app.route("/api/submit_task", methods=["POST"])
@requires_auth
def submit_task():
    incoming = request.get_json()

    success, id = Task.add_task(
        incoming.get("task"),
        incoming.get("user_id"),
        incoming.get("status")
    )

    if not success:
        return jsonify(message="Error submitting task", id=None), 409

    return jsonify(success=True, id=id)



@app.route("/api/get_tasks_for_user", methods=["POST"])
@requires_auth
def get_tasks_for_user():
    incoming = request.get_json()

    return jsonify(
        tasks=[i.serialize for i in Task.get_tasks_for_user(incoming["user_id"]).all()]
    )

@app.route("/api/delete_task", methods=["POST"])
@requires_auth
def delete_task():
    incoming = request.get_json()

    success = Task.delete_task(incoming.get('task_id'))
    if not success:
        return jsonify(message="Error deleting task"), 409

    return jsonify(success=True)


@app.route("/api/edit_task", methods=["POST"])
@requires_auth
def edit_task():
    incoming = request.get_json()
   
    success = Task.edit_task(
        incoming.get('task_id'),
        incoming.get('task'),
        incoming.get('status')
    )
    if not success:
        return jsonify(message="Error editing task"), 409

    return jsonify(success=True)
