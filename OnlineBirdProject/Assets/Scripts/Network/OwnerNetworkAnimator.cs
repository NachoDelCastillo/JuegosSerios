using Unity.Netcode.Components;

public class OwnerNetworkAnimator : NetworkAnimator
{
    // Esto se hace para que el cliente tambien pueda enviar cambios al servidor y sean aceptados
    // En este caso para actualizar las animaciones
    // De esta forma se sobrecarga menos al servidor de responsabilidades
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}