public class BookStack : GameStack
{
    protected override void ShowWarning(Warning w, GameSlot slot)
    {
        var message = w switch
        {
            Warning.Full => "A pilha já está cheia",
            Warning.Empty => "Não há o que retirar",
            Warning.GrabNotTop => "É possível apenas pegar o livro do topo",
            Warning.ReleaseNotTop => "Não pode colocar o livro fora do topo",
            Warning.WrongGroup => "Não pode colocar aqui",
        };
        base.ShowWarning(message, slot);
    }
}
