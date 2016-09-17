pragma solidity ^0.4.1;
import "./coin.sol";

contract ColorCoin is Coin(0){

    function ColorCoin(address exchangeContractAddress) Coin(exchangeContractAddress) { }

    function cashin(address receiver, uint amount) onlyowner {

        coinBalanceMultisig[receiver] += amount;

        CoinCashIn(receiver, amount);
    }

    // cashout coins (called only from exchange contract)
    function cashout(bytes32 operation, address from, uint amount, address to) onlyFromExchangeContract { 
        if (coinBalanceMultisig[from] < amount) {
            throw;
        }

        coinBalanceMultisig[from] -= amount;

        CoinCashOut(msg.sender, from, amount, to);
    }
}