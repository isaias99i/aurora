# Personagem em terceira pessoa

Protótipo offline em Godot 4 .NET e C#, com personagem `CharacterBody3D`, câmera orbital e cenário de teste feito apenas com primitivas do Godot.

## Abrir e executar

1. Use **Godot 4.7.2 .NET** e um SDK .NET compatível com `net8.0`. Essa é a versão já configurada no projeto; a edição do Godot sem .NET não executa C#.
2. No gerenciador do Godot, importe o arquivo `project.godot` desta pasta.
3. Abra `Main.tscn`, compile pelo botão **Build** do editor e pressione **F5**. A cena principal já está configurada. **F6** também funciona com `Main.tscn` aberta.
4. O mouse é capturado ao iniciar. Se estiver solto, clique na área do jogo para capturá-lo.

Para compilar pelo terminal, dentro desta pasta:

```powershell
dotnet build PWPrototype.csproj
```

## Controles

| Entrada | Ação |
| --- | --- |
| W / A / S / D | Movimento relativo à orientação horizontal da câmera |
| Mouse | Girar a câmera em torno do personagem |
| Espaço | Pular quando estiver no chão |
| Esc | Liberar o cursor e suspender os comandos de movimento |
| Clique esquerdo | Capturar o cursor e retomar o controle |

Ao perder o foco da janela, o cursor também é liberado. A gravidade e as colisões continuam funcionando com o cursor solto. Não há pulo duplo nem pulo automático ao manter Espaço pressionado.

## Configuração no Inspector

Selecione o nó raiz de `Player.tscn` (ou a instância `Player` em `Main.tscn`):

| Propriedade exportada | Padrão | Unidade / efeito |
| --- | --- | --- |
| Speed | 6 | Metros por segundo |
| Acceleration | 30 | Metros por segundo ao quadrado, para acelerar e frear |
| Jump Velocity | 6 | Velocidade vertical inicial em metros por segundo |
| Mouse Sensitivity | 0,003 | Radianos por pixel; valores menores tornam o mouse mais lento |

A gravidade usa **Project Settings → Physics → 3D → Default Gravity** (9,8 m/s²). A distância da câmera fica em `CameraPivot/SpringArm3D → Spring Length` (5 m). A inclinação vertical é limitada entre −65° e 35°. O marcador amarelo indica a frente da cápsula.

## Teste de aceite no editor

1. Execute com **F5**: a cápsula azul deve aparecer, cair a pequena distância inicial e permanecer sobre o chão.
2. Use **WASD**, solte as teclas e alterne direções: o personagem deve acelerar e frear suavemente. Compare W com W+D; a diagonal não deve aumentar a velocidade.
3. Gire a câmera cerca de 90° e pressione W: o movimento deve acompanhar a nova direção da câmera. Olhar para cima ou para baixo não deve alterar a velocidade horizontal.
4. Pressione **Espaço**, solte e pressione novamente durante o salto: deve haver apenas um pulo, seguido de queda e pouso. Mantenha Espaço pressionado ao pousar: não deve pular novamente sozinho.
5. Caminhe contra paredes e blocos: o personagem deve ser bloqueado ou deslizar pela superfície, sem atravessá-la. Pule sobre os blocos menores para testar o pouso elevado.
6. Suba e desça a rampa azul à direita: o personagem deve acompanhar a inclinação e permanecer apoiado quando parar.
7. Perto de uma parede, gire a câmera para colocá-la entre a parede e o personagem: a câmera deve se aproximar, recuperando a distância ao sair do obstáculo. Mova o mouse bastante na vertical para verificar os limites de inclinação.
8. Pressione **Esc**, mova o mouse e tente WASD: os comandos ficam suspensos. Clique para retomá-los. Alterne para outra janela e volte para verificar a liberação do cursor ao perder o foco.
9. Pare com **F8**, ajuste **Speed** e **Mouse Sensitivity** no Inspector e execute novamente para comparar. Confira se o painel **Debugger** permanece sem erros.

## Arquivos

- `Player.cs` — movimento em passos de física, gravidade, pulo, giro visual, câmera e captura do mouse.
- `Player.tscn` — personagem reutilizável com cápsula visível, colisão e `CameraPivot → SpringArm3D → Camera3D`.
- `Main.tscn` — arena de 24 × 24 m, paredes de 4 m, blocos de 0,6 / 1,2 / 2 m, rampa de 20°, iluminação e instância do personagem.
- `project.godot` — cena inicial, ações de entrada, camadas de colisão e renderizador Compatibility.

O mundo usa a camada de física 1 e o personagem a camada 2. A câmera testa apenas o mundo e exclui o corpo do jogador. A câmera é filha direta do braço para usar a forma de colisão derivada de seu plano próximo, conforme a [documentação do SpringArm3D](https://docs.godotengine.org/en/stable/tutorials/3d/spring_arm.html). O mouse usa [ScreenRelative](https://docs.godotengine.org/en/stable/classes/class_inputeventmousemotion.html#class-inputeventmousemotion-property-screen-relative) para manter a sensibilidade independente da escala da janela.

Não há assets externos ou bibliotecas adicionais. O escopo contém somente o personagem e seu ambiente de teste, sem combate, inventário ou multiplayer.
