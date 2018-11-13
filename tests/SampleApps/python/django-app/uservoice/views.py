from django.http import HttpResponse
from datetime import datetime

def home(request):
    return HttpResponse('Hello, World! from Uservoice app' + str(datetime.now()))