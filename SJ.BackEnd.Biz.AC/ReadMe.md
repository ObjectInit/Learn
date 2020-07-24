# AC业务对象说明
## 对象列表

|Class|CName|父类|DClass|ClassData|备注|
|--|--|
|AcSubRec|科目对账|BizTableCode|AcSubRec|"cname", "科目对账"|重写了TableName：account|
|AcReconRef |科目对账与科目关系 | PubOthers| AcReconRef|"cname", "科目对账与科目关系", "vision", 1 |
|AcPay | | | | "cname", "付款头", "ApproveFlow", "z03", "vision", 1| |
|AcPayD | | | | | |
|AcReceipt | | | | | |
|AcReceiptD | | | | | |
|AcVoucherDSub | | | | | |
|AcReco | | | | | |
|AcRecoD | | | | | |
|AcAnalySet | | | | | |
|-- todo -- | | | | | |

## 业务对象定义说明：

### AcSubRec：科目对账

#### 预留字段定义

|  属性 |  名称|物理字段 | 类型|说明|
| ----- | --- |----- |---- |---- |
| t0 | 下设xxx    |t0 | 开关| 根据科目已启用Tcode，用于凭证分录筛选时验证该字段是否一致 |
| t1 | 下设xxx    |t1| 开关| 根据科目已启用Tcode |
| tn | 下设xxx    |tn| 开关| 根据科目已启用Tcode |

#### 子查询定义

|属性|名称||
|---|--|
| | | |

### AcReconRef ：科目对账与科目关系

#### 预留字段定义

|  属性 |  名称|物理字段 | 类型|说明|
| ----- | --- |----- |---- |---- |
| subrec | 科目对账    |a9 | 关联| 关联AcSubRec(科目对账) |

