.PHONY: build run

default: run

build: ## installs the dependencies based on the requirements file
	@pip install -r requirements.txt
	cd static
	@npm install
	@npm run build

run: ## runs the app
	@echo "starting the application ..."
	@python manage.py runserver
