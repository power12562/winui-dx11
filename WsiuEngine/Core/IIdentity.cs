using System;

namespace WsiuEngine.Core
{
    /// <summary>
    /// 고유 식별성을 가진 객체의 최상위 인터페이스입니다.    <br/>
    /// 단순 데이터가 아닌, 독립적인 '엔티티' 또는 '에셋'임을 나타냅니다.
    /// </summary>
    public interface IIdentity
    {
        /// <summary>
        /// 객체가 독립적인 엔티티로 동작하는지 여부입니다.<br/>
        /// true일 경우 직렬화 시 데이터 전체가 아닌 UId 참조로 저장됩니다.
        /// </summary>
        bool IsEntity { get; }

        /// <summary>
        /// 객체의 고유 식별자(UID)입니다.<br/>
        /// Entity의 고유 ID를 반환하며, 없을 경우 Guid.Empty를 반환합니다.
        /// </summary>
        Guid UId { get; }
    }
}
