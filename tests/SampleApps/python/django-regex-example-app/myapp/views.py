from django.http import HttpResponse
from django.shortcuts import render

def homePageView(request):
    return render(request, 'home/index.html', {'msg':'Hello World'})
