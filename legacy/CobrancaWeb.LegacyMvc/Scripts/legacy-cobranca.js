/**
 * CobrançaWeb — Módulo Operacional Legado
 * Script: legacy-cobranca.js (jQuery 3.x / Bootstrap 3)
 * 
 * Descrição:
 * Script frontend clássico de manipulação de DOM e chamadas AJAX legado (ASP.NET MVC).
 * Demonstra a lógica client-side acoplada, validações inline e chamadas aos endpoints
 * do ConsultaCobrancaController.
 */

$(document).ready(function () {
    var diasAtrasoAtual = 0;

    // 1. Abrir Modal de Negociação a partir da linha da tabela
    $('.btn-abrir-acordo').on('click', function () {
        var btn = $(this);
        var contratoId = btn.data('id');
        var numeroContrato = btn.data('numero');
        var nomeDevedor = btn.data('devedor');
        var valorAtualizado = parseFloat(btn.data('valor'));
        diasAtrasoAtual = parseInt(btn.data('dias'), 10);

        // Preenche os campos do modal
        $('#txtContratoId').val(contratoId);
        $('#lblNumeroContrato').text(numeroContrato);
        $('#lblNomeDevedor').text(nomeDevedor);
        $('#lblDiasAtraso').text(diasAtrasoAtual + ' dias');
        $('#lblValorAtualizado').text('R$ ' + valorAtualizado.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

        // Reseta estado dos inputs e alertas
        $('#txtDesconto').val(10);
        $('#cboParcelas').val('3');
        $('#alertaErroNegociacao').hide().text('');
        $('#quadroResultadoSimulacao').hide();
        $('#btnEfetivarAcordo').prop('disabled', true);

        // Exibe o modal
        $('#modalNegociacao').modal('show');
    });

    // 2. Validação client-side preventiva ao digitar o desconto
    $('#txtDesconto').on('input change', function () {
        var desc = parseFloat($(this).val()) || 0;
        if (diasAtrasoAtual < 90 && desc > 30) {
            $('#alertaErroNegociacao')
                .text('Atenção: A política de crédito limita o desconto a 30% para contratos com menos de 90 dias de atraso.')
                .show();
        } else {
            $('#alertaErroNegociacao').hide().text('');
        }
        // Invalida a simulação prévia caso o desconto mude
        $('#quadroResultadoSimulacao').hide();
        $('#btnEfetivarAcordo').prop('disabled', true);
    });

    $('#cboParcelas').on('change', function () {
        $('#quadroResultadoSimulacao').hide();
        $('#btnEfetivarAcordo').prop('disabled', true);
    });

    // 3. Simular Acordo (AJAX POST)
    $('#btnSimularAcordo').on('click', function () {
        var contratoId = $('#txtContratoId').val();
        var desconto = parseFloat($('#txtDesconto').val()) || 0;
        var parcelas = parseInt($('#cboParcelas').val(), 10);

        if (!contratoId) {
            alert('Erro: Contrato não selecionado.');
            return;
        }

        // Bloqueia botão durante a requisição
        var $btn = $(this);
        $btn.prop('disabled', true).html('<span class="glyphicon glyphicon-refresh glyphicon-refresh-animate"></span> Calculando...');

        $.ajax({
            url: '/ConsultaCobranca/SimularAcordo',
            type: 'POST',
            dataType: 'json',
            data: {
                contratoId: contratoId,
                descontoPercentual: desconto,
                parcelas: parcelas
            },
            success: function (res) {
                if (res.Sucesso) {
                    $('#alertaErroNegociacao').hide().text('');

                    var totalFormatado = 'R$ ' + parseFloat(res.ValorAcordo).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                    var parcelaFormatada = res.Parcelas + 'x de R$ ' + parseFloat(res.ValorParcela).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

                    $('#lblTotalAcordo').text(totalFormatado);
                    $('#lblValorParcela').text(parcelaFormatada);
                    $('#quadroResultadoSimulacao').slideDown();

                    // Habilita o botão de efetivação
                    $('#btnEfetivarAcordo').prop('disabled', false);
                } else {
                    $('#quadroResultadoSimulacao').hide();
                    $('#btnEfetivarAcordo').prop('disabled', true);
                    $('#alertaErroNegociacao').text(res.Mensagem || 'Não foi possível simular o acordo com estes parâmetros.').show();
                }
            },
            error: function (xhr, status, error) {
                $('#quadroResultadoSimulacao').hide();
                $('#btnEfetivarAcordo').prop('disabled', true);
                $('#alertaErroNegociacao').text('Erro de comunicação com o servidor: ' + (xhr.responseText || error)).show();
            },
            complete: function () {
                $btn.prop('disabled', false).html('<span class="glyphicon glyphicon-refresh"></span> Calcular Proposta');
            }
        });
    });

    // 4. Efetivar Acordo (Transação manual no servidor)
    $('#btnEfetivarAcordo').on('click', function () {
        var contratoId = $('#txtContratoId').val();
        var desconto = parseFloat($('#txtDesconto').val()) || 0;
        var parcelas = parseInt($('#cboParcelas').val(), 10);

        if (!confirm('Confirma o fechamento deste acordo com o devedor? As parcelas e o novo status serão gravados imediatamente.')) {
            return;
        }

        var $btn = $(this);
        $btn.prop('disabled', true).html('<span class="glyphicon glyphicon-refresh glyphicon-refresh-animate"></span> Processando...');

        $.ajax({
            url: '/ConsultaCobranca/EfetivarAcordo',
            type: 'POST',
            dataType: 'json',
            data: {
                contratoId: contratoId,
                descontoPercentual: desconto,
                quantidadeParcelas: parcelas
            },
            success: function (res) {
                if (res.Sucesso) {
                    alert('Acordo formalizado com sucesso!\nProtocolo/ID: ' + res.NegociacaoId + '\nValor Acordado: R$ ' + parseFloat(res.ValorAcordado).toFixed(2));
                    $('#modalNegociacao').modal('hide');
                    window.location.reload();
                } else {
                    $('#alertaErroNegociacao').text('Falha ao efetivar acordo: ' + (res.Mensagem || 'Erro desconhecido.')).show();
                    $btn.prop('disabled', false).html('<span class="glyphicon glyphicon-ok"></span> Confirmar e Firmar Acordo');
                }
            },
            error: function (xhr, status, error) {
                $('#alertaErroNegociacao').text('Erro ao processar acordo no servidor: ' + (xhr.responseText || error)).show();
                $btn.prop('disabled', false).html('<span class="glyphicon glyphicon-ok"></span> Confirmar e Firmar Acordo');
            }
        });
    });
});
