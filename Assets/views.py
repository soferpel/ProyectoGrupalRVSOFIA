from django.shortcuts import render, get_object_or_404
from django.http import HttpResponse
from .models import Empresa, Trabajador


# Vistas basadas en funciones





def listaEmpresas(request):
    return HttpResponse('primera vista')


































def inicio(request):
    return HttpResponse('HOLA MUNDO')


def despedida(request):
    return HttpResponse('ADIOS')


def fecha(request):
    return HttpResponse('16/10/2023')


def listaEmpresas(request): 
    empresas = Empresa.objects.order_by('nombre')
    cadenaDeTexto = ', '.join([e.nombre for e in empresas])
    return HttpResponse(cadenaDeTexto)


def listaTrabajadores(request):  # Para mostrar la lista de trabajadores
    trabajadores = Trabajador.objects.all()
    cadenaDeTexto = "Lista de Trabajadores:\n"

    if trabajadores.exists():
        for trabajador in trabajadores:
            cadenaDeTexto += (
                f"- ID: {trabajador.id}, "
                f"Nombre: {trabajador.nombre}, "
                f"Fecha de Nacimiento: {trabajador.fecha_nacimiento}, "
                f"Antigüedad: {trabajador.antiguedad} años, "
                f"Empresa: {trabajador.empresa.nombre}\n"
            )
    else:
        cadenaDeTexto += "No hay trabajadores registrados."
    return HttpResponse(cadenaDeTexto)


def detalleEMPRESA(request, id_empresa):
    empresa = get_object_or_404(Empresa, pk=id_empresa)
    trabajadores = empresa.trabajadores.all()
    cadenaDeTexto = f"{empresa.nombre} - CIF: {empresa.cif}\n"
    cadenaDeTexto += "Trabajadores:\n"

    if trabajadores.exists():
        for trabajador in trabajadores:
            cadenaDeTexto += f"- {trabajador.nombre}, Antigüedad: {trabajador.antiguedad} años\n"
    else:
        cadenaDeTexto += "No hay trabajadores asociados a esta empresa."
    return HttpResponse(cadenaDeTexto)




def index(request):
    return render(request, 'index.html')


def listaTrabajadores2(request):
    trabajadores = Trabajador.objects.order_by('nombre')
    contexto = {'lista_trabajadores': trabajadores}
    return render(request, 'listaT.html', contexto)


def detalleTrabajador(request, id_trabajador):
    trabajador = get_object_or_404(Trabajador, pk=id_trabajador)
    contexto = {'trabajador': trabajador}
    return render(request, 'detalleTrabajador.html', contexto)


def detalleTrabajadorConPlantillas(request, id_trabajador):
    trabajador = get_object_or_404(Trabajador, pk=id_trabajador)
    contexto = {'trabajador': trabajador}
    return render(request, 'detalleTrabajador.html', contexto)


def detalleEmpresaConPlantillas(request, id_empresa):
    empresa = get_object_or_404(Empresa, pk=id_empresa)
    contexto = {'empresa': empresa}
    return render(request, 'detalle.html', contexto)


def listaEmpresasConPlantillas(request):
    empresas = Empresa.objects.order_by('nombre')
    contexto = {'empresa_list': empresas}
    return render(request, 'lista.html', contexto)


def detalle(request, nombre_empresa):
    return HttpResponse(f"Consultando la empresa {nombre_empresa}.")


def nosehacerviews(request, id_empresa):
    return HttpResponse(f"Información de la empresa con ID = {id_empresa}.")
