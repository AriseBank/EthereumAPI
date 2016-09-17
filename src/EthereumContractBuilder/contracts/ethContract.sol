import "./coin.sol";

contract EthCoin is Coin(0) {

    function EthCoin(address exchangeContractAddress) Coin(exchangeContractAddress) { }

    function cashin(address receiver, uint amount) onlyowner {

        coinBalanceMultisig[receiver] += msg.value;

        CoinCashIn(receiver, msg.value);
    }

    function cashout(bytes32 operation, address from, uint amount, address to) onlyFromExchangeContract checkClientApprove(operation, from) { 
        if (coinBalanceMultisig[from] < amount) {
            throw;
        }

        coinBalanceMultisig[from] -= amount;

        to.send(amount);

        CoinCashOut(msg.sender, from, amount, to);
    }
}